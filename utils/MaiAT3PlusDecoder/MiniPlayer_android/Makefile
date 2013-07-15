outdir=../output

target=$(outdir)/MiniPlayer

objs=$(outdir)/MaiWaveOut.o\
	$(outdir)/MiniPlayer.o

libat3pd=MaiAT3PlusDecoder
libat3pdn=$(outdir)/lib$(libat3pd).a

Mai_CXX_O=g++ -O2 "-I." "-I../include" "-L$(outdir)" -fno-exceptions -fno-rtti
Mai_RM_O=rm -f

all : $(target)

$(target) : $(objs) $(libat3pdn)
	$(Mai_CXX_O) -o $(target) $(objs) -l$(libat3pd) -lOpenSLES

$(objs) : 
$(outdir)/%.o : %.cpp
	$(Mai_CXX_O) -c -o $@ $<

$(libat3pdn) :
	cd ../src && make

clean :
	$(Mai_RM_O) $(objs)

