# SplitZip

This is the tool to split big zip file into small ones.  Below example shows how to split original zip into smaller 100MB ones. 

```
> SplitZip.exe MyZipFile.zip 100
Writing MyZipFile_000.zip ... 102,952,578 bytes
Writing MyZipFile_001.zip ... 104,485,091 bytes
Writing MyZipFile_002.zip ...  85,873,981 bytes
```

## ARM Template

This is ARM Template to illustrate upload those as multi-parts.

```json
{
  "properties": {
    "template": {
      "$schema": "http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
      "contentVersion": "1.0.0.0",
      "resources": [
        {
          "name": "mysite/Zip/MyZipFile_000",
          "type": "Microsoft.Web/sites/extensions/multi-parts",
          "apiVersion": "2018-02-01",
          "location": "Central US",
          "properties": {
            "packageUri": "https://mycdn.net/public/MyZipFile_000.zip"
          }
        },
        {
          "name": "mysite/Zip/MyZipFile_001",
          "type": "Microsoft.Web/sites/extensions/multi-parts",
          "apiVersion": "2018-02-01",
          "location": "Central US",
          "dependsOn": [
            "Microsoft.Web/sites/mysite/extensions/Zip/multi-parts/MyZipFile_000"
          ],
          "properties": {
            "packageUri": "https://mycdn.net/public/MyZipFile_001.zip"
          }
        },
        {
          "name": "mysite/Zip/MyZipFile_002",
          "type": "Microsoft.Web/sites/extensions/multi-parts",
          "apiVersion": "2018-02-01",
          "location": "Central US",
          "dependsOn": [
            "Microsoft.Web/sites/mysite/extensions/Zip/multi-parts/MyZipFile_001"
          ],
          "properties": {
            "packageUri": "https://mycdn.net/public/MyZipFile_002.zip"
          }
        }
      ]
    },
    "mode": "Incremental"
  }
}
```
