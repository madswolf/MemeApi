FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /MemeApi

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release --property:PublishDir=out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN echo "deb http://deb.debian.org/debian/ bookworm main contrib" > /etc/apt/sources.list && \
    echo "deb-src http://deb.debian.org/debian/ bookworm main contrib" >> /etc/apt/sources.list && \
    echo "deb http://security.debian.org/ bookworm-security main contrib" >> /etc/apt/sources.list && \
    echo "deb-src http://security.debian.org/ bookworm-security main contrib" >> /etc/apt/sources.list
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update; apt-get install -y ttf-mscorefonts-installer fontconfig

RUN apt-get update && \
    apt-get install -y \
      ttf-mscorefonts-installer \
      fontconfig \
      python3 \
      python3-pil \
      python3-requests

RUN ln -s /usr/bin/python3 /usr/bin/python
COPY /MemeApi/test.py .
WORKDIR /MemeApi
COPY --from=build-env /MemeApi/MemeApi/out .
ENTRYPOINT ["dotnet", "MemeApi.dll"]