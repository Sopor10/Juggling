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
if [ ! -f audio.mp3 ]; then
    AUDIO_FILE=$(ls *.mp3 | head -n 1)
    if [ -n "$AUDIO_FILE" ]; then
        echo "Renaming $AUDIO_FILE to audio.mp3"
        mv "$AUDIO_FILE" audio.mp3
    fi
fi

# Ensure all files in the working directory have Linux line endings
# Specifically title.txt and any .melt files
dos2unix title.txt 2>/dev/null
find . -maxdepth 1 -name "*.melt" -exec dos2unix {} + 2>/dev/null
TITLE=$(cat title.txt | tr -d '\r')

# Ensure all files in the common directory also have Linux line endings
dos2unix "$COMMON"/* 2>/dev/null

# Overlay title on intro
# Check if name contains an accent - if so we can't use Obelix so use a similar font
if [[ $TITLE == *[äöüàèìòùáéíóú]* ]]; then
    FONT="$COMMON/bangers.regular.ttf"
else
    FONT="$COMMON/ObelixProB-cyr.ttf"
fi
ffmpeg -i $COMMON/PZ-INTRO-without-pattern-name.avi -vf "drawtext=fontfile=$FONT: enable='gte(t,1.5)': text='$TITLE': fontcolor=white: fontsize=80: x=(w-text_w)/2: y=(h-text_h-80) + (text_h+80)*(2.5-min(t\,2.5))" -c:a copy -y PZ-intro.mp4

# Trim and fade audio.mp3 here because melt needs exact times (optional: if no audio, original video audio is kept)
USE_EXTERNAL_AUDIO=0
if [ -f audio.mp3 ]; then
    USE_EXTERNAL_AUDIO=1
    duration=$(ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 video.mp4)
    # Intro is 5s, Outro is 6.5s
    audio_duration=$(echo $duration + 5 + 6.5 | bc)
    non_fade=$(echo $audio_duration - 2 | bc)
    ffmpeg -i audio.mp3 -t $audio_duration -acodec copy -y trimmed_audio.mp3
    ffmpeg -i trimmed_audio.mp3 -af "afade=t=out:st=$non_fade:d=2" -y final_audio.mp3
    rm trimmed_audio.mp3
fi

# Generate Outro
# Use environment variables if provided, otherwise fallback to defaults
# TITLE is already read from title.txt above
LOCATION=${LOCATION:-"Goirle December 2025"}
JUGGLERS=${JUGGLERS:-"Lars"}
MUSICARTIST=${MUSICARTIST:-"Someone"}

# Font logic (same as intro)
# FONT is already determined in the intro section above
FONT_OUTRO="$FONT"
# Purple color: #4B0082
# Duration: 6.5 seconds

# Layout parameters
BASE_HEADER_SIZE=90
BASE_INFO_SIZE=35
SPEED=400
MAX_WIDTH=1200 # 80% of 1920
SPACING=${BLOCK_SPACING:-300} # Default spacing between pairs
INTERNAL_GAP=${INTERNAL_SPACING:-75}
# Measure all lines at base size to calculate scale
W1=$(ffmpeg -f lavfi -i "color=c=black:s=2000x200:d=0.1" -vf "drawtext=fontfile=$FONT_OUTRO:text='$TITLE':fontsize=$BASE_HEADER_SIZE" -f null - 2>&1 | grep -oP "t:.*text_w:\K[0-9]+")
W2=$(ffmpeg -f lavfi -i "color=c=black:s=2000x200:d=0.1" -vf "drawtext=fontfile=$FONT_OUTRO:text='juggled@ $LOCATION':fontsize=$BASE_INFO_SIZE" -f null - 2>&1 | grep -oP "t:.*text_w:\K[0-9]+")
W3=$(ffmpeg -f lavfi -i "color=c=black:s=2000x200:d=0.1" -vf "drawtext=fontfile=$FONT_OUTRO:text='Jugglers':fontsize=$BASE_HEADER_SIZE" -f null - 2>&1 | grep -oP "t:.*text_w:\K[0-9]+")
W4=$(ffmpeg -f lavfi -i "color=c=black:s=2000x200:d=0.1" -vf "drawtext=fontfile=$FONT_OUTRO:text='$JUGGLERS':fontsize=$BASE_INFO_SIZE" -f null - 2>&1 | grep -oP "t:.*text_w:\K[0-9]+")
W5=$(ffmpeg -f lavfi -i "color=c=black:s=2000x200:d=0.1" -vf "drawtext=fontfile=$FONT_OUTRO:text='Music by':fontsize=$BASE_HEADER_SIZE" -f null - 2>&1 | grep -oP "t:.*text_w:\K[0-9]+")
W6=$(ffmpeg -f lavfi -i "color=c=black:s=2000x200:d=0.1" -vf "drawtext=fontfile=$FONT_OUTRO:text='$MUSICARTIST':fontsize=$BASE_INFO_SIZE" -f null - 2>&1 | grep -oP "t:.*text_w:\K[0-9]+")

# Find max width
MAX_W=0
for w in $W1 $W2 $W3 $W4 $W5 $W6; do
    if [ "$w" -gt "$MAX_W" ]; then MAX_W=$w; fi
done

# Calculate scale factor (1.0 if fits, else MAX_WIDTH/MAX_W)
SCALE=$(echo "if ($MAX_W > $MAX_WIDTH) $MAX_WIDTH / $MAX_W else 1.0" | bc -l)

HEADER_SIZE=$(echo "$BASE_HEADER_SIZE * $SCALE" | bc)
INFO_SIZE=$(echo "$BASE_INFO_SIZE * $SCALE" | bc)

# Vertical internal gap (between Header and Subtitle)


# Adjust vertical positions based on scale and spacing
# Block 1 (Title)
Y1=$(echo "-$SPACING * $SCALE" | bc)
Y2=$(echo "(-$SPACING + $INTERNAL_GAP + 30) * $SCALE" | bc)

# Block 2 (Jugglers)
Y3=$(echo "0 * $SCALE" | bc)
Y4=$(echo "($INTERNAL_GAP + 30) * $SCALE" | bc)

# Block 3 (Music)
Y5=$(echo "$SPACING * $SCALE" | bc)
Y6=$(echo "($SPACING + $INTERNAL_GAP + 30) * $SCALE" | bc)

# Offset for animation (y = max(CENTER_POS, h - SPEED*t + OFFSET))
# We calculate OFFSET so that at t=0, the text starts below the screen or in sequence
O1=$(echo "50 * $SCALE" | bc)
O2=$(echo "220 * $SCALE" | bc)
O3=$(echo "450 * $SCALE" | bc)
O4=$(echo "620 * $SCALE" | bc)
O5=$(echo "850 * $SCALE" | bc)
O6=$(echo "1020 * $SCALE" | bc)

ffmpeg -f colorspace=all=bt709:range=tv -f lavfi -i "color=c=#321D5B:s=1920x1080:d=6.5" \
-vf "drawtext=fontfile=$FONT_OUTRO:text='$TITLE':fontcolor=white:fontsize=$HEADER_SIZE:x=(w-text_w)/2:y=max((h-text_h)/2+$Y1\,h-$SPEED*t+$O1), \
     drawtext=fontfile=$FONT_OUTRO:text='juggled@ $LOCATION':fontcolor=white:fontsize=$INFO_SIZE:x=(w-text_w)/2:y=max((h-text_h)/2+$Y2\,h-$SPEED*t+$O2), \
     drawtext=fontfile=$FONT_OUTRO:text='Jugglers':fontcolor=white:fontsize=$HEADER_SIZE:x=(w-text_w)/2:y=max((h-text_h)/2+$Y3\,h-$SPEED*t+$O3), \
     drawtext=fontfile=$FONT_OUTRO:text='$JUGGLERS':fontcolor=white:fontsize=$INFO_SIZE:x=(w-text_w)/2:y=max((h-text_h)/2+$Y4\,h-$SPEED*t+$O4), \
     drawtext=fontfile=$FONT_OUTRO:text='Music by':fontcolor=white:fontsize=$HEADER_SIZE:x=(w-text_w)/2:y=max((h-text_h)/2+$Y5\,h-$SPEED*t+$O5), \
     drawtext=fontfile=$FONT_OUTRO:text='$MUSICARTIST':fontcolor=white:fontsize=$INFO_SIZE:x=(w-text_w)/2:y=max((h-text_h)/2+$Y6\,h-$SPEED*t+$O6)" \
-c:v libx264 -t 6.5 -pix_fmt yuv420p -y outro.mp4

# Combine using melt (with external audio mix or video-only to keep original audio)
if [ "$USE_EXTERNAL_AUDIO" -eq 1 ]; then
    cp "$COMMON/project.melt" ./project.melt
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
melt -v -quiet melt_file:"$MELT" -consumer avformat:output.mp4 acodec=aac vcodec=libx264 pix_fmt=yuv420p b=12000k quality=high+ width=1920 height=1080 preset=slow profile=high crf=18
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

