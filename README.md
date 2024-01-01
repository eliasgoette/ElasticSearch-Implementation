# ElasticSearch-Implemtation

Demo implementation of ElasticSearch for NoSQL lecure

## Requirements:
- Docker
- ElasticSearch container
- ElasticSearch.NET NuGet package

### Container Setup
#### Official documentation:
https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html

#### Terminal commands:
```
docker network create elastic
docker pull docker.elastic.co/elasticsearch/elasticsearch:8.11.3
```

With https:
```
docker run --name elasticsearch --net elastic -p 9200:9200 -it -m 1GB docker.elastic.co/elasticsearch/elasticsearch:8.11.3
```

Without https (for development):
```
docker run --name elasticsearch --net elastic -p 9200:9200 -it -m 1GB -e "discovery.type=single-node" -e "xpack.security.enabled=false" docker.elastic.co/elasticsearch/elasticsearch:8.11.3
```
