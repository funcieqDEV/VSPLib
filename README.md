# VSPLib
The official package manager for V#

### usage
`vsp install name|link`

**example usage**
`vsp install http`

for now you can use http module

```
extern ("http.dll") as http

server = http.createServer("/root")//its your static folder
server.Start()
```

as you can see () means you are using installed module; if you not use() then it will search for http.dll in you directory