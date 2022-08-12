#!/bin/bash

echo "**********************************************"

mongo mongo:27017 <<EOF
var cfg = {
    "_id": "rs0",
    "version": 1,
    "members": [
{% for serverip in groups['ubuntu'] %}
        {
{% if serverip == inventory_hostname %}
            "_id": 0,
            "host": "mongo:27017",
            "priority": 2,
{% else %}
            "_id": {{ loop.index }},
            "host": "{{ serverip }}:27017",
            "priority": 0,
{% endif %}
        },
{% endfor %}
    ]
};

rs.initiate(cfg);
rs.reconfig(cfg);
quit();
