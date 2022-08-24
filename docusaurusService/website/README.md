# Website

This website is built using [Docusaurus 2](https://v2.docusaurus.io/), a modern static website generator.

## Installation

install node.js LTS version

install [yarn](https://classic.yarnpkg.com/lang/en/docs/install/#windows-stable)
## Build  
To build your proto you should run folowwing commands from "website" directory:  
This command will add protos to generating queue
```cmd
  protoc --doc_out=./fixtures --doc_opt=json,proto_workspace.json --proto_path=../../Protos ClientProtos/authServiceForClient.proto ClientProtos/briefcaseServiceForClient.proto ClientProtos/orderServiceForClient.proto ClientProtos/productServiceForClient.proto ClientProtos/balanceServiceForClient.proto decimalValue.proto 

```
> To add new .proto file for generating you should add to this command proto's path from "Protos" directory in root path  

This command will generate docs for protos in queue
```cmd
npx docusaurus generate-proto-docs
```

## Run
To run docs you should run following command from "website" directory:  
```cmd
npm run start
```
To stop docs press CTRL+C
