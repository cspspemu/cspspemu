docker run -it --rm -v $(pwd):/data/build bkcsoft/pspsdk sh -c "cd common; make"
docker run -it --rm -v $(pwd):/data/build bkcsoft/pspsdk sh -c "cd tests/cpu/cpu_alu; make"

