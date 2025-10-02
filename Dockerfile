# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# Add build argument for publish directory
ARG PUBLISH_DIR

# Error out if PUBLISH_DIR is not set
RUN if [ -z "$PUBLISH_DIR" ]; then \
    echo "ERROR: PUBLISH_DIR build argument is required" && exit 1; \
    fi

RUN apk add --no-cache libc6-compat

# Copy the contents of the publish directory to '/azuremcpserver' and set it as the working directory
RUN mkdir -p /azuremcpserver
COPY ${PUBLISH_DIR} /azuremcpserver/
WORKDIR /azuremcpserver

# List the contents of the current directory
RUN ls -la

RUN if [ ! -f "azmcp" ]; then \
    echo "ERROR: azmcp executable does not exist" && exit 1; \
    fi

ENTRYPOINT ["./azmcp", "server", "start"]
