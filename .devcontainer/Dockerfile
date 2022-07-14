# this image is built and updated weekly
# https://github.com/cse-labs/codespaces-images

#checkov:skip=CKV_DOCKER_2: No healthcheck is needed 
#checkov:skip=CKV_DOCKER_3: No user is needed 
#checkov:skip=CKV_DOCKER_5: Update is used in same line as upgrade 
#checkov:skip=CKV_DOCKER_7: Alright to use "latest" tag

FROM ghcr.io/cse-labs/k3d:latest

# some images require specific values
ARG USERNAME=vscode

# log docker build start
RUN echo "$(date +'%Y-%m-%d %H:%M:%S')    docker build start" >> "/home/${USERNAME}/status"

RUN apt-get update && \
    apt-get upgrade -y

# change ownership
RUN chown -R ${USERNAME}:${USERNAME} "/home/${USERNAME}"

# log completion - make sure this is last
RUN echo "$(date +'%Y-%m-%d %H:%M:%S')    docker build complete" >> "/home/${USERNAME}/status"
