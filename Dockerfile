# The minimal base image for self-contained Trimmed SingleFile azmcp executable
FROM debian:12.12-slim AS runtime

ARG PUBLISH_DIR

RUN if [ -z "$PUBLISH_DIR" ]; then \
    echo "ERROR: PUBLISH_DIR build argument is required" && exit 1; \
    fi

# Install minimal dependencies to run azmcp executable
# 1. ca-certificates - For HTTPS connections to Azure
# 2. libc6 - Core C library
# 3. libicu-dev - Unicode/globalization support
#
RUN apt-get update && apt-get install -y \
    ca-certificates \
    libc6 \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# Create and configure tmp directory with proper permissions for .NET single-file extraction
# See: https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview
RUN mkdir -p /tmp && chmod 1777 /tmp
ENV DOTNET_BUNDLE_EXTRACT_BASE_DIR=/tmp
ENV HOME=/tmp

# Create non-root user to run azmcp server
RUN groupadd -r azmcpuser && useradd -r -g azmcpuser azmcpuser

# Create directory to host azmcp executable and set ownership
RUN mkdir -p /azuremcpserver && chown azmcpuser:azmcpuser /azuremcpserver

# Copy the azmcp executable from the publish directory
COPY ${PUBLISH_DIR} /azuremcpserver/

# Change to azuremcpserver directory and set ownership to azmcpuser
WORKDIR /azuremcpserver
RUN chown -R azmcpuser:azmcpuser /azuremcpserver

# List the contents of the current directory (i.e., /azuremcpserver)
RUN ls -la

RUN if [ ! -f "azmcp" ]; then \
    echo "ERROR: azmcp executable does not exist" && exit 1; \
    fi

# Grant execute permission to azmcp executable
RUN chmod +x azmcp

# Switch to non-root azmcpuser
USER azmcpuser

ENTRYPOINT ["./azmcp", "server", "start"]
