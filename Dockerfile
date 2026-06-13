FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /MemeApi

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release --property:PublishDir=out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Enable multiverse (Ubuntu equivalent of Debian's contrib) for ttf-mscorefonts-installer
RUN apt-get update && apt-get install -y software-properties-common && \
    add-apt-repository -y multiverse && \
    echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y \
      ttf-mscorefonts-installer \
      fontconfig \
      python3 \
      python3-pil \
      python3-requests && \
    rm -rf /var/lib/apt/lists/*

RUN ln -s /usr/bin/python3 /usr/bin/python
COPY /MemeApi/renderImage.py .
WORKDIR /MemeApi
COPY --from=build-env /MemeApi/MemeApi/out .
ENTRYPOINT ["dotnet", "MemeApi.dll"]