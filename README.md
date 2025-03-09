# botsms - Recebedor de SMS para confirmações de contas

## Canais a serem utilizados
|CANAL|
| --- |
|TELEGRAM|
|WHATSAPP|

## 1º API Provedoras de SMS
### https://sms-activate.org/

## 2º API Provedora de SMS
### https://sms-man.com/pt </br>

## Detalhes
### Chaves API's serão todas definidas no env da aplicação
### env terá que ter o ID de cada canal que o bot participa para enviar avisos
### Webhooks serão definidos no env

## Comandos admin
### Lógica
### /admin.senha.comando.parametro1.parametro2

### Adicionar voucher
### /admin.senha.addvoucher.quantidade.valor

### Banir usuário
### /admin.senha.userban.motivo.userid
### Motivo pode ser: fraud | spam

### Desbanir usuário
### /admin.senha.userunban.userid