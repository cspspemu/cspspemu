
.PHONY: all clean

CC = gcc
CFLAGS = -Wall -g

TARGET = libkirk.a
OBJS = kirk_engine.o SHA1.o AES.o ec.o bn.o 

all: $(TARGET)

$(TARGET): $(OBJS)
	ar rcs $@ $(OBJS)

clean:
	rm -f *.o $(TARGET)

