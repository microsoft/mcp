# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# Add build argument for publish directory
ARG PUBLISH_DIR

# Error out if PUBLISH_DIR is not set
RUN if [ -z "$PUBLISH_DIR" ]; then \
    echo "ERROR: PUBLISH_DIR build argument is required" && exit 1; \
    fi

# Add build argument for executable name
ARG EXECUTABLE_NAME

# Error out if EXECUTABLE_NAME is not set
RUN if [ -z "$EXECUTABLE_NAME" ]; then \
    echo "ERROR: EXECUTABLE_NAME build argument is required" && exit 1; \
    fi

RUN apk add --no-cache libc6-compat

# Copy the contents of the publish directory to '/mcp-server' and set it as the working directory
RUN mkdir -p /mcp-server
COPY ${PUBLISH_DIR} /mcp-server/
WORKDIR /mcp-server

# List the contents of the current directory
RUN ls -la

# Ensure the server binary exists
RUN if [ ! -f $EXECUTABLE_NAME ]; then \
    echo "ERROR: $EXECUTABLE_NAME executable does not exist" && exit 1; \
    fi
    
# Copy the server binary to a known location and make it executable
COPY ${PUBLISH_DIR}/${EXECUTABLE_NAME} server-binary
RUN chmod +x server-binary && test -x server-binary

ENTRYPOINT ["./server-binary", "server", "start"]
