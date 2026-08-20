FROM debian:bookworm-slim@sha256:abd67ffcfa541b485a3dff59865ab629aa048a6c613e639d36e7456b0b229241

ARG ASTERISK_VERSION=22.10.1
ARG ASTERISK_SHA256=0953564c44fa49827f3c9d70ca6e80db83828c9848440852c6be44c961855353

RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        build-essential \
        bzip2 \
        ca-certificates \
        curl \
        espeak-ng \
        ffmpeg \
        libedit-dev \
        libedit2 \
        libjansson-dev \
        libjansson4 \
        libncurses-dev \
        libsrtp2-1 \
        libsrtp2-dev \
        libsqlite3-0 \
        libsqlite3-dev \
        libssl-dev \
        libssl3 \
        liburiparser-dev \
        liburiparser1 \
        libuuid1 \
        libxml2 \
        libxml2-dev \
        patch \
        pkg-config \
        uuid-dev \
    && curl --fail --show-error --location \
        "https://downloads.asterisk.org/pub/telephony/asterisk/asterisk-${ASTERISK_VERSION}.tar.gz" \
        --output /tmp/asterisk.tar.gz \
    && echo "${ASTERISK_SHA256}  /tmp/asterisk.tar.gz" | sha256sum --check --strict \
    && mkdir /tmp/asterisk-source \
    && tar --extract --gzip --file /tmp/asterisk.tar.gz \
        --directory /tmp/asterisk-source --strip-components=1 \
    && cd /tmp/asterisk-source \
    && ./configure --with-pjproject-bundled \
    && make -j2 \
    && make install \
    && cd / \
    && rm -rf /tmp/asterisk-source /tmp/asterisk.tar.gz \
    && apt-get purge -y --auto-remove \
        build-essential bzip2 libedit-dev libjansson-dev libncurses-dev \
        libsrtp2-dev libsqlite3-dev libssl-dev liburiparser-dev libxml2-dev \
        patch pkg-config uuid-dev \
    && rm -rf /var/lib/apt/lists/*

COPY asterisk.conf /etc/asterisk/asterisk.conf
COPY modules.conf /etc/asterisk/modules.conf
COPY logger.conf /etc/asterisk/logger.conf
COPY http.conf /etc/asterisk/http.conf
COPY rtp.conf /etc/asterisk/rtp.conf
COPY extensions.conf /etc/asterisk/extensions.conf
COPY ari.conf.template /opt/ivr-lab/ari.conf.template
COPY pjsip.conf.template /opt/ivr-lab/pjsip.conf.template
COPY entrypoint.sh /usr/local/bin/ivr-lab-entrypoint
COPY healthcheck.sh /usr/local/bin/ivr-lab-healthcheck

RUN sed -i 's/\r$//' /usr/local/bin/ivr-lab-entrypoint /usr/local/bin/ivr-lab-healthcheck \
    && chmod 0755 /usr/local/bin/ivr-lab-entrypoint /usr/local/bin/ivr-lab-healthcheck

EXPOSE 5060/udp 8088/tcp 10000-10020/udp

ENTRYPOINT ["/usr/local/bin/ivr-lab-entrypoint"]
