# santedb-sdk
SanteDB SDK Toolkit for Developing Applets / Plugins

## SQLCipher

In order to compile the SDK from source, you will need to place a binary copy of SqlCipher.dll into the `Solution Items` folder. SqlCipher can be compiled using C/C++ compiler (like Microsoft Visual C++). We have included an almagamated copy of SqlCipher on the SanteDB community page https://github.com/santedb/SqlCipher-Amalgamated. 

Alternately, if you do not require encryption support for SQLite , you can place any copy of SQLite.dll named `SqlCipher.dll` into the `Solution Items` folder.
