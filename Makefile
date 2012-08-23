LIBDIRS=-lib:../../sdk/aws-dotnet-sdk/AWS\ SDK\ for\ .NET/bin

.PHONY: all clean

all: GlacierTool.exe

clean:
	-rm -rf GlacierTool.exe GlacierTool.exe.mdb

GlacierTool.exe: GlacierTool.cs
	gmcs -langversion:future -sdk:4 -platform:anycpu -debug+ -optimize+ \
		$(LIBDIRS) -r:AWSSDK -out:GlacierTool.exe GlacierTool.cs
