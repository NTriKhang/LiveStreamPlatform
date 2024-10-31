from pymongo import MongoClient
from fastapi import FastAPI
from bson import ObjectId
from datetime import datetime
# Example of passing this ratings_dict to the Surprise library

from surprise import Dataset
from surprise import Reader
import pandas as pd
from surprise import SVD
from surprise.model_selection import train_test_split
import redis
import json 

import sys
import hashlib

sys.path.append('./Tools/DynamicKey/AgoraDynamicKey/python/src')
from Tools.DynamicKey.AgoraDynamicKey.python.src.RtcTokenBuilder2 import RtcTokenBuilder, Role_Subscriber # type: ignore

app_id = "c1b98689c58b489a9988c2eac3a65f5e"
app_certificate = "4f3398988a4f41d0b23b641d30a5bb92"

app = FastAPI()

client = MongoClient('mongodb+srv://caohieeu2003:5qqJ1I5aVbOVfd0X@educationlivesream.gbct01s.mongodb.net/?retryWrites=true&w=majority&appName=EducationLivesream')
db = client['LiveStreamDb']
watch_train_model = db['WatchTrainModel']
redis_client = redis.Redis(host='redis-19011.c295.ap-southeast-1-1.ec2.redns.redis-cloud.com', port=19011, password='TMz4UJQfAXAEhjEZinBDse448hGFSTyl')

def convert_doc_for_json(doc):
    if isinstance(doc, dict):
        return {k: convert_doc_for_json(v) for k, v in doc.items()}
    elif isinstance(doc, list):
        return [convert_doc_for_json(i) for i in doc]
    elif isinstance(doc, ObjectId):
        return str(doc)
    elif isinstance(doc, datetime):
        return doc.isoformat()  # Convert datetime to ISO 8601 string format
    else:
        return doc

ratings_dict = {
    "user_id": [],
    "item_id": [],
    "rating": []
}

@app.get("/")
def read_root():

    return "asd"

@app.get("/train")
def read_train():
    documents = list(watch_train_model.find())
    documents = [convert_doc_for_json(doc) for doc in documents]

    for document in documents:
        user_id = document['_id']  # This is the user_id
        interactions = document['Interactions']  # This is the list of interactions
        
        # Loop through all interactions of this user
        for interaction in interactions:
            video_id = interaction['videoId']
            play_time = interaction['playTime']  # This will be treated as the rating

            # Append data to ratings_dict
            ratings_dict['user_id'].append(user_id)
            ratings_dict['item_id'].append(video_id)
            ratings_dict['rating'].append(play_time)
    
    df = pd.DataFrame(ratings_dict)
    reader = Reader(rating_scale=(1, 10)) 
    data = Dataset.load_from_df(df[['user_id', 'item_id', 'rating']], reader)
    trainset, testset = train_test_split(data, test_size=0.2)
    model = SVD()
    model.fit(trainset)
    predictions = model.test(testset)

    results = [{"user_id": pred.uid, "item_id": pred.iid, "predicted_rating": pred.est} for pred in predictions]

    redis_client.set('svd_predictions', json.dumps(results))

    return {"message": "Predictions stored in Redis", "predictions_count": len(results)}

@app.get("/GetToken")
def getToken(UserId: str, channel_name: str):
    token_expiration_in_seconds = 3600
    privilege_expiration_in_seconds = 3600

    hash_object = hashlib.sha256(UserId.encode())
    hash_hex = hash_object.hexdigest()
    user_id_hash_int = int(hash_hex, 16)

    if not app_id or not app_certificate:
        return {"error": "Need to set AGORA_APP_ID and AGORA_APP_CERTIFICATE"}

    token = RtcTokenBuilder.build_token_with_uid(app_id, app_certificate, channel_name, user_id_hash_int, Role_Subscriber,
                                                 token_expiration_in_seconds, privilege_expiration_in_seconds)
    return {"token": token}