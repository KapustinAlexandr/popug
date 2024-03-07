FROM registry.access.redhat.com/ubi9 AS ubi-micro-build
RUN mkdir -p /mnt/rootfs
RUN dnf install --installroot /mnt/rootfs curl --releasever 9 --setopt install_weak_deps=false --nodocs -y && \
    dnf --installroot /mnt/rootfs clean all && \
    rpm --root /mnt/rootfs -e --nodeps setup

FROM quay.io/keycloak/keycloak:23.0.7
COPY --from=ubi-micro-build /mnt/rootfs /

# Event listner, который пишет события в кафку
# https://github.com/SnuK87/keycloak-kafka
ADD --chown=keycloak:keycloak keycloak-kafka-1.1.5-jar-with-dependencies.jar  /opt/keycloak/providers/keycloak-kafka-1.1.5-jar-with-dependencies.jar

RUN /opt/keycloak/bin/kc.sh build

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
