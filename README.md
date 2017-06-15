# QueryString

## About

Web API Using QueryString like OData $filter

filter Parse Query Prototype.

usecase
```
GET / http://xxx/api/resource?filter=(Car eq 'SuperCar') or (Car eq 'SmallCar') and (CreateDate le '2013/03/12 13:12:11')
```

```
POST / http://xxx/api/resource

{
  "filter" : "(Car eq 'SuperCar') or (Car eq 'SmallCar') and (CreateDate le '2013/03/12 13:12:11')"
}
```

Reference by https://github.com/aanufriyev/IdentityDirectory

Copyright f7q © 2017