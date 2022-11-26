# SharpBot
``` yaml  
version: "3"

services:
  sharpbot:
    image: crinte/sharpbot:master
    restart: unless-stopped
    container_name: sharpbot
    volumes:
      - ./sharpbot/config:/app/config
    ports:
      - 80:5115
```  