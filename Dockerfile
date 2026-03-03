FROM ubuntu:22.04

# Add arguments to set timezone and dotnet version
ARG DOTNET_IMAGE_VERSION
ARG DOTNET_TIME_ZONE
ARG DOTNET_LANG_NAME
ARG DOTNET_LANG_INPUTFILE
ARG DOTNET_LANG_CHARMAP

# Set environment variables for non-interactive installation
ENV DEBIAN_FRONTEND=${DOTNET_DEBIAN_FRONTEND}
ENV TZ=${DOTNET_TIME_ZONE}

# Install base packages, build-essential libreadline-dev zlib1g-dev for make install postgresql-client specific version
RUN apt update && apt install -y wget locales gnupg2 apt-transport-https \
    ca-certificates curl software-properties-common \
    libnss3-tools iputils-ping telnet net-tools \
    openssl tzdata build-essential libreadline-dev zlib1g-dev

# Add the Microsoft package signing key to your list of trusted keys and add the package repository
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

# Update package list and install .NET SDK
RUN apt update && apt install -y dotnet-sdk-${DOTNET_IMAGE_VERSION}.0

# Install dotnet-ef tool for dotnet core specific version
RUN dotnet tool install --global dotnet-ef --version ${DOTNET_IMAGE_VERSION}.*

# Clean apt cache
RUN rm -rf /var/cache/apt && apt-get clean

# Set locale to specified language
RUN localedef -i ${DOTNET_LANG_INPUTFILE} -c -f ${DOTNET_LANG_CHARMAP} -A /usr/share/locale/locale.alias ${DOTNET_LANG_NAME}

# Set timezone to Asia/Taipei
RUN ln -sf /usr/share/zoneinfo/${DOTNET_TIME_ZONE} /etc/localtime

# Configure timezone non-interactively
RUN echo "${DOTNET_TIME_ZONE}" > /etc/timezone && dpkg-reconfigure -f noninteractive tzdata

# Clear package lists
RUN rm -rf /var/lib/apt/lists/*

# Switch to application directory
WORKDIR /app

# Copy project files
COPY . .

# Publish web application
RUN dotnet publish api/HR.LeaveManagement.Api/HR.LeaveManagement.Api.csproj -c Release -o /deploy/api

# Switch to deploy directory
WORKDIR /deploy/api

# Run the app
ENTRYPOINT [ "dotnet", "HR.LeaveManagement.Api.dll" ]