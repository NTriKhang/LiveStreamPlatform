FROM tiangolo/nginx-rtmp

# Install FFmpeg and AWS CLI
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    apt-get install -y python3-pip && \
    pip3 install awscli && \
    apt-get clean

COPY ["rtmp/nginx.conf", "/etc/nginx/nginx.conf"]
COPY ["rtmp/transcode_ffmpeg.sh", "/tmp/bin/transcode_ffmpeg.sh"]
# COPY ["rtmp/aws_syn_s3.sh", "/tmp/bin/aws_syn_s3.sh"]
