GlacierTool
===========

Description
-----------
GlacierTool is a command line utility for accessing
[Amazon Glacier](https://aws.amazon.com/glacier/), the long-term storage service
of Amazon Web Services.

Implementation
--------------
GlacierTool is written in C# and is primarily intended to be run on Linux under
the Mono runtime. It should be relatively trivial to build on other Posix
platforms that support Mono, as well as on Windows under the official .NET
implementation.

Current Features
----------------
* Upload archives

Upcoming Features
-----------------
* Download archives
* Delete archives
* Create and delete vaults
* View vault inventory

Building
--------
Instructions for Windows compilation with Visual Studio are left as an exercise
for the reader.

The [Amazon Web Services SDK for .NET](http://aws.amazon.com/sdkfornet/) is a
prerequisite for building and running GlacierTool. The most recent version (at
the time of this writing) is distributed in the project's root directory as
`AWSSDK.dll`. The Makefile can be easily modified if for some reason this file
is located elsewhere.

On Posix systems, first ensure that [Mono](http://www.mono-project.com/) is
installed. Run `make` in the project directory to compile `GlacierTool.exe`.

Running
-------
To run GlacierTool on a Posix System, run `mono GlacierTool.exe`. Running the
program without arguments will print usage information.

To perform any action with Amazon Glacier, you will need your Amazon Web
Services account ID, access key, and secret key, which can be found on the
[security credentials](https://portal.aws.amazon.com/gp/aws/securityCredentials)
page of the Amazon Web Services site after logging in.

Known Issues
------------
The Mono runtime on Posix systems does not ship with SSL/TLS root certificates
installed, which means that connections to Amazon Web Services over HTTPS tend
to fail with the cryptic error message, `the authentication or decryption has
failed`. The Mono [security FAQ](http://www.mono-project.com/FAQ:_Security) has
suggestions on ways to fix this. The easy way is to simply run
`mozroots --import`, which will install all of Mozilla's root certificates into
Mono's trust store. To do this for all users on the machine, instead run
`mozroots --import --machine` as root.

Notes
-----
Be aware that performing transactions with GlacierTool will result in fees being
charged to your Amazon account, according to the pricing structure set out by
Amazon. See their [pricing page](https://aws.amazon.com/glacier/pricing/) for
specifics on their pricing policies.

The developer disclaims responsibility for charges resulting from erroneous
program behavior. Be careful, and double-check before you commit anything.

License
-------
This project is distributed under the terms of the
[simplified BSD license](http://opensource.org/licenses/BSD-2-Clause). See the
LICENSE file for details.
