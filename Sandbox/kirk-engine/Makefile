.PHONY: all clean build_lib clean_lib

all: build_lib build_ipldecrypt build_test_harness

clean: clean_ipldecrypt clean_lib clean_test_harness

build_lib:
	make -C libkirk -f Makefile all
	
clean_lib:
	make -C libkirk -f Makefile clean
	
build_ipldecrypt:
	make -C ipl_decrypt -f Makefile all

build_test_harness:
	make -C test_harness -f Makefile all
	
clean_ipldecrypt:
	make -C ipl_decrypt -f Makefile clean

clean_test_harness:
	make -C test_harness -f Makefile clean
