# Api de monitoramento de status

# Sobre o projeto

Api desenvolvida usando C#, ASP.NET, EF, Sql Server, RabbitMq e redis. Foi utilizada uma arquitetura monolítica orientada a eventos e em cache para otmização do tempo de respota para o usuário. A ideia central do projeto é monitorar a saúde de um determinado endpoint verificando se este está online ou offline. O monitoramento é feito em um background service que é executado a cada minuto. Ao ser verificada uma url, caso seu status mude, é simulado um envio de email para o usuário dono da url cadastrada.

>[!NOTE]
>Como o projeto não está com deploy o envio de email é simulado por um método interno da api que gera um log no console da aplicação com as informações da verificação. O objetivo era praticar uma solução com mensageria para otimizar o tempo de monitoramento das urls.

Existem 3 endpoints principais:
- auth
- user
- url

O endpoint "auth" faz o gerenciamento de login e refresh ambas utilizando jwt. O token ao efetuar o login com sucesso tem uma duração mais curta de 3 minutos enquanto o refresh token tem uma validade de 20 minutos.

As rotas "user" são protegidas com autorização, exceto o endpoint "/user" com o método http POST responsável por criar um usuário novo.Ou seja, o usuário só pode acessá-las utilizando um token de acesso válido. Os outros endpoints são os de deletar a conta e o de recuperar as informações do usuário pelo seu id.

Em "url" todas as rotas são protegidas com autorização. É possível um usuário cadastrar uma nova url, editar ou apagar. É possível também recuperar as informações de uma url por seu id, ou recuperar uma lista de urls do usuário pelo id do usuário.

Para uma documentação mais detalhada foi utilizado Scalar como interface gráfica sendo acessível em 
```bash
http://localhost:5062/scalar
```

O projeto conta com uma cobertura de testes unitários para todos os serviços, entidades e controllers

# Executando o projeto

>[!NOTE]
>Para executar o projeto é necessário ter o .NET 9 instalado

1 - Primeiro clone o projeto e navegue até a pasta criada
```bash
git clone https://github.com/gabrielferreira02/healthcheck-api.git
cd healthcheck-api
```

2 - Para rodar os testes esteja dentro do projeto de testes ou na raiz da pasta criada e execute
```bash
dotnet test
```

3 - Para iniciar a api navegue até o projeto da api
```bash
cd HealthCheckApi
```

4 - Inicialize o docker compose que contém o banco de dados, o rabbitmq e o cache com redis
```bash
docker compose up -d
```

5 - As migrações já estão geradas no projeto, utilize o EF Core para implementar na base de dados
```bash
dotnet ef database update
```

6 - Agora basta iniciar a api utilizando
```bash
dotnet run
```