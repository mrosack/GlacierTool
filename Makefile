# uncomment and change this if AWSSDK.DLL is not in this directory
#AWSSDK_DIR=-lib:dir_to_AWSSDK.dll

.PHONY: all clean

all: GlacierTool.exe

clean:
	-rm -rf GlacierTool.exe GlacierTool.exe.mdb

GlacierTool.exe: GlacierTool.cs
	gmcs -langversion:future -sdk:4 -platform:anycpu -debug+ -optimize+ \
		$(AWSSDK_DIR) -r:AWSSDK -out:GlacierTool.exe GlacierTool.cs
