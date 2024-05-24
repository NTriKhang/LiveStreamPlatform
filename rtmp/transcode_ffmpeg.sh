# #!/bin/bash

stream_key="$1"
rtmp_url_in="rtmp://localhost/live/$stream_key"
rtmp_url_out_720p="rtmp://localhost/hls/${stream_key}_720p2628kbs"
rtmp_url_out_480p="rtmp://localhost/hls/${stream_key}_480p1128kbs"
rtmp_url_out_360p="rtmp://localhost/hls/${stream_key}_360p878kbs"
rtmp_url_out_240p="rtmp://localhost/hls/${stream_key}_240p528kbs"

ffmpeg -i "$rtmp_url_in" -c:v libx264 -preset veryfast -tune zerolatency -vf "scale=1280x720" -c:a aac -f flv "$rtmp_url_out_720p" &
ffmpeg -i "$rtmp_url_in" -c:v libx264 -preset veryfast -tune zerolatency -vf "scale=854x480" -c:a aac -f flv "$rtmp_url_out_480p" &
ffmpeg -i "$rtmp_url_in" -c:v libx264 -preset veryfast -tune zerolatency -vf "scale=640x360" -c:a aac -f flv "$rtmp_url_out_360p" &
ffmpeg -i "$rtmp_url_in" -c:v libx264 -preset veryfast -tune zerolatency -vf "scale=426x240" -c:a aac -f flv "$rtmp_url_out_240p" &