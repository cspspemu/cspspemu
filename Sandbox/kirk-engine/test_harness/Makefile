.PHONY: all clean

LIBDIR :=  ../libkirk
CC = gcc
CFLAGS = -Wall -g
TARGET = test_harness
LIBS = -lkirk
OBJS = main.o

all: $(TARGET)

clean:
	rm -f *.o $(TARGET)
	
$(TARGET): $(OBJS)
	$(CC) $(OBJS) -L$(LIBDIR) -o $(TARGET) $(LIBS)


	
	
	
