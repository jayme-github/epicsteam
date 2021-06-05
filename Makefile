CC=mcs
CFLAGS=-pkg:dotnet

epicsteam.exe: epicsteam.cs
	$(CC) $(CFLAGS) epicsteam.cs

clean:
	rm -f epicsteam.exe