FROM mcr.microsoft.com/devcontainers/base:ubuntu-24.04

# initialize-host.sh supplies the optional, environment-specific CA in the build context.
# The empty file used when the CA is unavailable is deliberately a no-op.
COPY ca.pem /tmp/microsandbox.crt
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates openssl \
    && if openssl x509 -in /tmp/microsandbox.crt -noout >/dev/null 2>&1; then \
         install -m 0644 /tmp/microsandbox.crt /usr/local/share/ca-certificates/microsandbox.crt; \
         update-ca-certificates; \
       fi \
    && rm -f /tmp/microsandbox.crt \
    && rm -rf /var/lib/apt/lists/*

# Keep Node/npm on the normal system trust store. If the optional CA was valid,
# update-ca-certificates added it there; otherwise this is the stock trust store.
ENV NODE_EXTRA_CA_CERTS=/etc/ssl/certs/ca-certificates.crt
