# Build the runtime image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:10.0.0-rc.1-azurelinux3.0-distroless AS runtime

ARG PUBLISH_DIR
ARG TARGETOS
ARG TARGETARCH

# Copy the contents of the publish directory to '/azuremcpserver' and set it as the working directory
COPY ${PUBLISH_DIR}/${TARGETOS}-${TARGETARCH}/dist/ /azuremcpserver/
WORKDIR /azuremcpserver

ENTRYPOINT ["./azmcp", "server", "start"]
