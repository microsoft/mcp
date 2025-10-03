# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0.8-bookworm-slim AS runtime

# Add build argument for publish directory
ARG PUBLISH_DIR
ARG EXECUTABLE_NAME

# Error out if PUBLISH_DIR is not set
RUN if [ -z "$PUBLISH_DIR" ]; then \
    echo "ERROR: PUBLISH_DIR build argument is required" && exit 1; \
    fi

# Copy the contents of the publish directory to '/server' and set it as the working directory
RUN mkdir -p /server
COPY ${PUBLISH_DIR} /server/
WORKDIR /server

# List the contents of the current directory
RUN ls -la

ENV EXECUTABLE_NAME=${EXECUTABLE_NAME}

RUN if [ ! -f "$EXECUTABLE_NAME" ]; then \
    echo "ERROR: $EXECUTABLE_NAME executable does not exist" && exit 1; \
    fi

ENTRYPOINT ["./$EXECUTABLE_NAME", "server", "start"]
