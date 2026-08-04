# Passing Zone video maker

This is a simple script to make videos for [Passing Zone](http://passing.zone), along with setups for videos I've uploaded.

## Installation

Install dependencies
```
sudo apt install texlive-xetex imagemagick ffmpeg melt
mkdir -p ~/.fonts
cp common/*.ttf ~/.fonts
fc-cache -f -v
```

Clone repo
```
git clone https://github.com/AdGold/passing-zone-videos.git
```

## Usage

Create a folder with the following files
* `title.txt`: the name of the patter
* `notation.tex`: the notation and credits as a latex document
* `video.mp4`: the video clip
* `audio.mp3` (optional): the music to add
* `project.melt` (optional): the melt file to define the project if non-standard (can normally be left out)

Run `./render.sh <folder name>`

This will create two files:
* `<title>.mp4` - the output video
* `<title> - notation.png` - the notation as a PNG
