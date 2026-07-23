#!/bin/bash

# Required setup:
# sudo apt install imagemagick ffmpeg melt
# mkdir -p ~/.fonts
# cp common/*.ttf ~/.fonts
# fc-cache -f -v

# Determine the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
COMMON="$SCRIPT_DIR/common"

# Strip carriage returns from arguments and input files
FOLDER=$(echo "$1" | tr -d '\r')
cd "$FOLDER" || exit 1

# Rename the uploaded video to video.mp4 if it's not already
# Program.cs saves files with their original names, but project.melt expects video.mp4
if [ ! -f video.mp4 ]; then
    VIDEO_FILE=$(ls *.mp4 | grep -v "PZ-intro.mp4" | grep -v "output.mp4" | head -n 1)
    if [ -n "$VIDEO_FILE" ]; then
        echo "Renaming $VIDEO_FILE to video.mp4"
        mv "$VIDEO_FILE" video.mp4
    fi
fi

# Rename the uploaded audio to audio.mp3 if it's not already
USE_EXTERNAL_AUDIO=0
if [ ! -f audio.mp3 ]; then
    AUDIO_FILE=$(ls *.mp3 | head -n 1)
    if [ -n "$AUDIO_FILE" ]; then
        echo "Renaming $AUDIO_FILE to audio.mp3"
        mv "$AUDIO_FILE" audio.mp3
        USE_EXTERNAL_AUDIO=1
    fi
else
    USE_EXTERNAL_AUDIO=1
fi

# Ensure all files in the working directory have Linux line endings
# Specifically title.txt and any .melt files
dos2unix title.txt 2>/dev/null
find . -maxdepth 1 -name "*.melt" -exec dos2unix {} + 2>/dev/null
TITLE=$(cat title.txt | tr -d '\r')

# Ensure all files in the common directory also have Linux line endings
dos2unix "$COMMON"/* 2>/dev/null

# Overlay title on intro (umlauts are transliterated in C# before render)
FONT="$COMMON/ObelixProB-cyr.ttf"
ffmpeg -i $COMMON/PZ-INTRO-without-pattern-name.avi -vf "drawtext=fontfile=$FONT: enable='gte(t,1.5)': text='$TITLE': fontcolor=white: fontsize=80: x=(w-text_w)/2: y=(h-text_h-80) + (text_h+80)*(2.5-min(t\,2.5))" -c:a copy -y PZ-intro.mp4

# Generate Outro
LOCATION=${LOCATION:-"Goirle December 2025"}
JUGGLERS=${JUGGLERS:-"Lars"}
MUSICARTIST=${MUSICARTIST:-""}

INTRO_DURATION=5
OUTRO_DURATION=6.5

video_duration=$(ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 video.mp4)
final_duration=$(echo $video_duration + $INTRO_DURATION + $OUTRO_DURATION | bc -l)

if [ "$USE_EXTERNAL_AUDIO" -eq 1 ]; then
    echo "Audio track will be trimmed to match total video duration: $final_duration seconds."
    non_fade=$(echo $final_duration - 2 | bc -l)
    ffmpeg -i audio.mp3 -vn -t $final_duration -y trimmed_audio.mp3
    ffmpeg -i trimmed_audio.mp3 -vn -af "afade=t=out:st=$non_fade:d=2" -y final_audio.mp3
    rm trimmed_audio.mp3
fi

TITLE_FONT="$COMMON/ObelixProB-cyr.ttf"
BODY_FONT="/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
if [ ! -f "$BODY_FONT" ]; then
    BODY_FONT="$TITLE_FONT"
fi

TITLE_SIZE=72
BODY_SIZE=36
MAX_TEXT_WIDTH=1000
TITLE_LINE_GAP=14
BODY_LINE_GAP=12
SECTION_GAP=36

escape_drawtext() {
    local s=$1
    s=${s//\\/\\\\}
    s=${s//:/\\:}
    s=${s//\'/\\\'}
    printf '%s' "$s"
}

measure_text_width() {
    local font=$1
    local size=$2
    local text=$3
    local width tmp
    tmp=$(mktemp)
    printf '%s' "$text" > "$tmp"
    # ImageMagick label render — ffmpeg no longer logs drawtext text_w
    width=$(convert -background none -fill white -font "$font" -pointsize "$size" \
        "label:@${tmp}" png:- 2>/dev/null | identify -format "%w" - 2>/dev/null || true)
    rm -f "$tmp"
    if [ -z "$width" ] || [ "$width" = "0" ]; then
        # Fallback estimate if convert fails (e.g. missing glyph metrics)
        width=$(printf '%s' "$text" | wc -c)
        width=$((width * size * 6 / 10))
    fi
    echo "$width"
}

# Word-wrap text to MAX_TEXT_WIDTH; prints one visual line per output line.
wrap_text() {
    local font=$1
    local size=$2
    local text=$3
    local max_w=${4:-$MAX_TEXT_WIDTH}

    if [ -z "$text" ]; then
        return
    fi

    local -a words
    read -r -a words <<< "$text"
    if [ ${#words[@]} -eq 0 ]; then
        return
    fi

    local current=""
    local word trial width
    for word in "${words[@]}"; do
        if [ -z "$current" ]; then
            trial="$word"
        else
            trial="$current $word"
        fi
        width=$(measure_text_width "$font" "$size" "$trial")
        if [ "$width" -gt "$max_w" ] && [ -n "$current" ]; then
            printf '%s\n' "$current"
            current="$word"
        else
            current="$trial"
        fi
    done
    if [ -n "$current" ]; then
        printf '%s\n' "$current"
    fi
}

is_placeholder() {
    local v
    v=$(echo "$1" | tr '[:upper:]' '[:lower:]')
    case "$v" in
        ""|"someone"|"unknown artist"|"unknown jugglers"|"unknown location"|"unknown title")
            return 0
            ;;
        *)
            return 1
            ;;
    esac
}

# Parallel arrays: OUTRO_FONTS / OUTRO_SIZES / OUTRO_TEXTS / OUTRO_GAPS_AFTER
OUTRO_FONTS=()
OUTRO_SIZES=()
OUTRO_TEXTS=()
OUTRO_GAPS=()

append_wrapped() {
    local font=$1
    local size=$2
    local text=$3
    local gap_after_last=${4:-$BODY_LINE_GAP}
    local line_gap=${5:-$BODY_LINE_GAP}

    local -a lines
    mapfile -t lines < <(wrap_text "$font" "$size" "$text")
    local i
    local count=${#lines[@]}
    for ((i = 0; i < count; i++)); do
        OUTRO_FONTS+=("$font")
        OUTRO_SIZES+=("$size")
        OUTRO_TEXTS+=("${lines[$i]}")
        if [ "$i" -eq $((count - 1)) ]; then
            OUTRO_GAPS+=("$gap_after_last")
        else
            OUTRO_GAPS+=("$line_gap")
        fi
    done
}

# Title
append_wrapped "$TITLE_FONT" "$TITLE_SIZE" "$TITLE" "$SECTION_GAP" "$TITLE_LINE_GAP"

# Location: split on last comma into place + date when possible
if ! is_placeholder "$LOCATION"; then
    if [[ "$LOCATION" == *,* ]]; then
        LOCATION_PLACE="${LOCATION%,*}"
        LOCATION_PLACE="${LOCATION_PLACE%"${LOCATION_PLACE##*[![:space:]]}"}"
        LOCATION_DATE="${LOCATION##*,}"
        LOCATION_DATE="${LOCATION_DATE#"${LOCATION_DATE%%[![:space:]]*}"}"
        append_wrapped "$BODY_FONT" "$BODY_SIZE" "juggled @ $LOCATION_PLACE" "$BODY_LINE_GAP"
        if [ -n "$LOCATION_DATE" ]; then
            append_wrapped "$BODY_FONT" "$BODY_SIZE" "$LOCATION_DATE" "$SECTION_GAP"
        else
            OUTRO_GAPS[$((${#OUTRO_GAPS[@]} - 1))]=$SECTION_GAP
        fi
    else
        append_wrapped "$BODY_FONT" "$BODY_SIZE" "juggled @ $LOCATION" "$SECTION_GAP"
    fi
fi

# Jugglers
if ! is_placeholder "$JUGGLERS"; then
    append_wrapped "$BODY_FONT" "$BODY_SIZE" "Jugglers: $JUGGLERS" "$SECTION_GAP"
fi

# Music
if ! is_placeholder "$MUSICARTIST"; then
    append_wrapped "$BODY_FONT" "$BODY_SIZE" "Music: $MUSICARTIST" "$BODY_LINE_GAP"
fi

# Compute total block height and starting Y (vertically centered)
TOTAL_HEIGHT=0
for ((i = 0; i < ${#OUTRO_TEXTS[@]}; i++)); do
    TOTAL_HEIGHT=$((TOTAL_HEIGHT + OUTRO_SIZES[i] + OUTRO_GAPS[i]))
done
# Remove trailing gap from last line for centering
if [ ${#OUTRO_GAPS[@]} -gt 0 ]; then
    TOTAL_HEIGHT=$((TOTAL_HEIGHT - OUTRO_GAPS[$((${#OUTRO_GAPS[@]} - 1))]))
fi

START_Y=$(( (1080 - TOTAL_HEIGHT) / 2 ))
if [ "$START_Y" -lt 40 ]; then
    START_Y=40
fi

# Build drawtext filter chain
VF_PARTS=()
CURRENT_Y=$START_Y
for ((i = 0; i < ${#OUTRO_TEXTS[@]}; i++)); do
    ESCAPED=$(escape_drawtext "${OUTRO_TEXTS[$i]}")
    VF_PARTS+=("drawtext=fontfile=${OUTRO_FONTS[$i]}:text='${ESCAPED}':fontcolor=white:fontsize=${OUTRO_SIZES[$i]}:x=(w-text_w)/2:y=${CURRENT_Y}")
    CURRENT_Y=$((CURRENT_Y + OUTRO_SIZES[i] + OUTRO_GAPS[i]))
done

VF_FILTER=$(IFS=,; echo "${VF_PARTS[*]}")

# Background: geometric purple (trianglify), fallback to solid color
BG_IMAGE="$COMMON/trianglify3.png"
if [ -f "$BG_IMAGE" ]; then
    ffmpeg -loop 1 -i "$BG_IMAGE" -vf "scale=1920:1080:force_original_aspect_ratio=increase,crop=1920:1080,setsar=1,${VF_FILTER}" \
        -t "$OUTRO_DURATION" -c:v libx264 -pix_fmt yuv420p -y outro.mp4
else
    ffmpeg -f lavfi -i "color=c=#321D5B:s=1920x1080:d=$OUTRO_DURATION" -vf "$VF_FILTER" \
        -c:v libx264 -pix_fmt yuv420p -y outro.mp4
fi

# Combine using melt (with external audio mix or video-only to keep original audio)
if [ "$USE_EXTERNAL_AUDIO" -eq 1 ]; then
    # Double check if final_audio.mp3 was actually created
    if [ -f final_audio.mp3 ]; then
        cp "$COMMON/project.melt" ./project.melt
    else
        echo "Warning: USE_EXTERNAL_AUDIO was 1 but final_audio.mp3 missing. Using video audio."
        cp "$COMMON/project_no_audio.melt" ./project.melt
    fi
else
    cp "$COMMON/project_no_audio.melt" ./project.melt
fi
MELT="project.melt"

# Ensure the melt file itself doesn't have CRLF
dos2unix "$MELT" 2>/dev/null

# Debug: Check files before melt
echo "Debug: Current directory: $(pwd)"
ls -l PZ-intro.mp4 video.mp4 "$MELT"
[ -f final_audio.mp3 ] && ls -l final_audio.mp3

# Check if melt is installed and its version
melt --version

# Run melt
echo "Running melt..."
# Force 16:9 display aspect. Without aspect=, melt inherits a wrong SAR from mixed
# clip sizes and pillarboxes even the 1920x1080 intro/outro.
melt -v -quiet melt_file:"$MELT" -consumer avformat:output.mp4 acodec=aac vcodec=libx264 pix_fmt=yuv420p b=12000k quality=high+ width=1920 height=1080 aspect=1.77778 preset=slow profile=high crf=18
MELT_EXIT_CODE=$?

if [ $MELT_EXIT_CODE -ne 0 ]; then
    echo "melt failed with exit code $MELT_EXIT_CODE"
    exit $MELT_EXIT_CODE
fi

if [ ! -f output.mp4 ]; then
    echo "Error: output.mp4 was not created by melt"
    exit 1
fi

echo "Render successful. Cleaning up..."
rm -f PZ-intro.mp4 outro.mp4 final_audio.mp3 project.melt
mv output.mp4 rendered_output.mp4

