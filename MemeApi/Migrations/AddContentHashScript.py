import requests
from dotenv import load_dotenv
import os
import json
from concurrent.futures import ThreadPoolExecutor, as_completed

load_dotenv()

API_HOST = os.getenv('API_HOST')
texts = requests.get(API_HOST + "texts/")
visuals = requests.get(API_HOST + "visuals/")
memes = requests.get(API_HOST + "memes/")

votables = []
if(not texts.ok or not visuals.ok or not memes.ok):
    print("failed")

texts = json.loads(texts.content)
visuals = json.loads(visuals.content)
memes = json.loads(memes.content)

for entry in texts:
    votables.append({"id":entry['id'], "type": "text"})

for entry in visuals:
    votables.append({"id":entry['id'], "type": "visual"})

for entry in memes:
    votables.append({"id":entry['id'], "type": "meme"})

def regenerate_contenthash_for_votable(entry):
    params = {
        'isMeme': entry['type'] == "meme"
    }
    id = entry["id"]
    response = requests.get(API_HOST + f'topics/votable/{id}/RegenerateContentHash', params=params)
    print(entry["type"], id, response.status_code, "content hash")

    if response.status_code not in (200, 404):
        print(response.status_code, "something went wrong")
        quit()

def verify_uniqueness_for_votable(entry):
    
    id = entry["id"]
    response = requests.get(API_HOST + f'topics/votable/{id}/verify')
    print(entry["type"], id, response.status_code, "verify")

    if response.status_code not in (200, 404):
        print(response.status_code, "something went wrong")
        quit()

# Run the processing of votables in parallel
with ThreadPoolExecutor() as executor:
    futures = [executor.submit(regenerate_contenthash_for_votable, entry) for entry in votables]
    for future in as_completed(futures):
        future.result()  # Ensures all threads complete

# Run the processing of votables in parallel
with ThreadPoolExecutor() as executor:
    futures = [executor.submit(verify_uniqueness_for_votable, entry) for entry in votables]
    for future in as_completed(futures):
        future.result()  # Ensures all threads complete