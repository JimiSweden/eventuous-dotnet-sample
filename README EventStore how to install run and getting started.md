

# Assuming you install the ESDB in C:\ESDB nd follow the instructions
Your folder strucure shuold look like this when you have created the certificates and a config file.
> 'es-gencert-cli.exe' has to be downloaded. 
```ps
PS C:\ESDB> dir  -Name -Recurse
Data
Index
Logs
es-gencert-cli.exe
eventstore.conf
certs
	certs\ca
	certs\node1
	certs\ca\ca.crt
	certs\ca\ca.key
	certs\node1\node.crt
	certs\node1\node.key
```

## Configuration
from 'https://configurator.eventstore.com/' Client connection tab
> Complete post-installation steps
> Copy node.crt and node.key files to C:\ESDB\certs on each node.
> (ca.crt and ca.key renamed to node.x)

> Copy ca.crt file to C:\ESDB\certs\ca on each node.
> Create a file called eventstore.conf using the node configuration from the section below, for each node.
> Place the file to the C:\ESDB directory of the node.

### Start the server by 
EventStore.ClusterNode.exe --config C:\ESDB\eventstore.conf

## Connection

Connection string:
```
esdb://admin:changeit@127.0.0.1:2113?tls=true

```
Note:

You either need to add the CA certificate to the trusted CA store of the client machine, or add &tlsVerifyCert=false to the connection string.

### Gettings started
https://developers.eventstore.com/clients/grpc/#connection-details



## certs. 
https://developers.eventstore.com/server/v21.10/security.html#certificate-generation-tool

The certificate store location is the location of the Windows certificate store, for example CurrentUser

### generate CA 
./es-gencert-cli create-ca

PS C:\ESDB> .\es-gencert-cli_1.1.0_Windows-x86_64\es-gencert-cli.exe create-ca
```
94mA CA certificate & key file have been generated in the './ca' directory
```

### generate node cert 

PS C:\ESDB> .\es-gencert-cli.exe create-node -ca-certificate ca/ca.crt -ca-key ca/ca.key -out ./node1  -ip-addresses 127.0.0.1 -dns-names localhost
```
94mA node certificate & key file have been generated in the './node1' directory
```

### import  (in Personal, Not local machine)
You can manually import it to the local CA cert store through Certificates Local Machine Management Console. 
To do that select Run from the Start menu, and then enter certmgr.msc. Then import certificate to Trusted Root Certification.
You can also run the PowerShell script instead:
Import-Certificate -FilePath ".\certs\ca\ca.crt" -CertStoreLocation Cert:\CurrentUser\Root
```
   PSParentPath: Microsoft.PowerShell.Security\Certificate::CurrentUser\Root

Thumbprint                                Subject
----------                                -------
ABC1234AED5977A6435778EDED7E390585840935  CN=EventStoreDB CA 12345670d831e3c2ec561b1894067135, O=Event Store Ltd, C=UK
```
### Certificate installation on a client environment (same as above, not needed if db and client is the same machine)
https://developers.eventstore.com/server/v21.10/security.html#certificate-installation-on-a-client-environment

## import the node cert in chrome, to access UI, https://127.0.0.1:2113/web/index.html#/
chrome://settings/security?search=cert 
chrome > settings > security > manage certificates > import. "C:\ESDB\certs\node1\node.crt"