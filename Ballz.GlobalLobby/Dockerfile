FROM alpine:edge
MAINTAINER Lukas Boersma <mail@lukas-boersma.com>

RUN echo "@testing http://dl-4.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories
RUN apk --update add mono@testing

EXPOSE 9162
ENV DOCKER_IS_RUNNING 1

ADD bin/Release/ /global-lobby

CMD sh -c "cd /global-lobby && mono ./Ballz.GlobalLobby.exe"
