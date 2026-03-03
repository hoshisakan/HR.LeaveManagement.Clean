FROM nginx:1.23.3

# Add arguments to set timezone
ARG NGINX_TIME_ZONE
ARG NGINX_LANG_NAME
ARG NGINX_LANG_INPUTFILE
ARG NGINX_LANG_CHARMAP
ARG DEBIAN_FRONTEND

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Install base packages
RUN apt update
RUN apt install -y locales wget gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools

# Set timezone to Asia/Taipei
RUN ln -sf /usr/share/zoneinfo/${NGINX_TIME_ZONE} /etc/localtime
# Reset tzdata software package let user set timezone take effect
RUN dpkg-reconfigure -f noninteractive tzdata

# Set locale to specified language
RUN localedef -i ${NGINX_LANG_INPUTFILE} -c -f ${NGINX_LANG_CHARMAP} -A /usr/share/locale/locale.alias ${NGINX_LANG_NAME}

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*