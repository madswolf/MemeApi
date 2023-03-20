import json
import time
import requests

# Opening JSON file
f = open('memes.json', encoding='utf-8')
  
# returns JSON object as 
# a dictionary
data = json.load(f)
  
baseurl = "https://media.mads.monster/visual/"
test_url = "https://localhost/memes"
# Iterating through the json
# list

meme_ids = []
toptext_ids = []
bottomtext_ids = []
for meme in data:
    visual = meme["visual"]
    if(not visual):
        #Not an actual meme
        continue

    visual_filename = visual["filename"]
    visual_file = requests.get(baseurl + visual_filename, allow_redirects=True)
    meme_ids.append(meme["id"])

    toptext = meme["topText"]
    if(toptext):
        toptext_ids.append(toptext["id"])
        toptext = toptext["memetext"]

    bottomtext = meme["bottomText"]
    if(bottomtext):
        bottomtext_ids.append(bottomtext["id"])
        bottomtext = bottomtext["memetext"]

    data = {"Toptext": toptext,"Bottomtext": bottomtext}
    session = requests.Session()
    session.verify = False
    response = session.post(test_url,files = {"VisualFile": visual_file.content}, data = data)
    #time.sleep(1)
    if(response.ok):
        print(meme["id"], visual_filename, toptext, bottomtext)
    else:
        print("OH FUCK")
  
print(len(meme_ids))
print(len(toptext_ids))
print(len(bottomtext_ids))
# Closing file
f.close()