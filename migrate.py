import json
import time
import requests
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
# Opening JSON file
f = open('memes.json', encoding='utf-8')
f_visual = open('visuals.json', encoding='utf-8')
f_toptext = open('toptexts.json', encoding='utf-8')
f_bottomtext = open('bottomtexts.json', encoding='utf-8')
# returns JSON object as 
# a dictionary
data_visuals = json.load(f_visual)
data_toptexts = json.load(f_toptext)
data_bottomtexts = json.load(f_bottomtext)
mediaurl = "https://media.mads.monster/visual/"
meme_url = "https://api.mads.monster/memes"
meme_by_id_url = "https://api.mads.monster/memes/ById"
text_url = "https://api.mads.monster/texts"
visual_url = "https://api.mads.monster/visuals"

visual_filename_to_ids_dict = {}
toptext_content_to_ids_dict = {}
bottomtext_content_to_ids_dict = {}
  
for visual in data_visuals:
    session = requests.Session()
    session.verify = False
    visual_file = requests.get(mediaurl + visual["filename"], allow_redirects=True)
    data = {"FileName":visual["filename"], "Topics":visual["topics"], "CreatedAt":visual["createdAt"]}
    session = requests.Session()
    session.verify = False
    response = session.post(visual_url,files = {"File": visual_file.content}, data = data)
    if(not response.ok):
        print(visual, response)
    else:
        visual_filename_to_ids_dict[visual["filename"]] = response.json()["id"]
        print("okay visual: ", visual)

for toptext in data_toptexts:
    session = requests.Session()
    session.verify = False
    response = session.post(text_url, data = json.dumps({"Text":toptext["text"], "Position": "TopText", "Topics":toptext["topics"], "CreatedAt":toptext["createdAt"]}), headers={'content-type': 'application/json'})
    if(not response.ok):
        print(toptext, response)
    else:
        if(toptext["text"] in toptext_content_to_ids_dict.keys()):
            print("duplicate")
        
        toptext_content_to_ids_dict[toptext["text"]] = response.json()["id"]        
        print("okay toptext: ", toptext)

for bottomtext in data_bottomtexts:
    session = requests.Session()
    session.verify = False
    response = session.post(text_url, data = json.dumps({"Text":bottomtext["text"], "Position": "BottomText", "Topics":bottomtext["topics"], "CreatedAt":toptext["createdAt"]}), headers={'content-type': 'application/json'})
    if(not response.ok):
        print(bottomtext, response)
    else:
        if(bottomtext["text"] in bottomtext_content_to_ids_dict.keys()):
            print("duplicate")
        
        bottomtext_content_to_ids_dict[toptext["text"]] = response.json()["id"]
        print("okay bottomtext: ", bottomtext)  

meme_ids = []
visuals_filenames = []
toptext_texts = []
bottomtext_texts = []
data = json.load(f)
for meme in data:
    print(meme)
    print(meme["memeVisual"])
    visual_filename = meme["memeVisual"]
    if(not visual_filename):
        #Not an actual meme
        continue
    session = requests.Session()
    session.verify = False
    if(visual_filename in visual_filename_to_ids_dict.keys()):
        print("existing")
        VisualId = visual_filename_to_ids_dict[visual_filename]
        toptext = meme["toptext"]
        bottomText = meme["bottomText"]
        toptextId = None
        bottomtextId = None
        if (toptext in toptext_content_to_ids_dict.keys()):
            toptextId = toptext_content_to_ids_dict[toptext]

        if (bottomText in toptext_content_to_ids_dict.keys()):
            toptextId = toptext_content_to_ids_dict[bottomText]

        if (toptext in toptext_content_to_ids_dict.keys()):
            bottomtextId = toptext_content_to_ids_dict[toptext]

        if (bottomText in toptext_content_to_ids_dict.keys()):
            bottomtextId = toptext_content_to_ids_dict[bottomText]
        
        data = {"VisualId": VisualId, "Topics":meme["topics"], "CreatedAt":meme["createdAt"]}
        if(toptextId):
            data["TopTextId"] = toptextId

        if(bottomtextId):
            data["BottomTextId"] = bottomtextId

        response = session.post(meme_by_id_url, json = data)
    else:
        visuals_filenames.append(visual_filename)
        visual_file = requests.get(mediaurl + visual_filename, allow_redirects=True)
        meme_ids.append(meme["id"])

        toptext = meme["toptext"]
        if(toptext):
            toptext_texts.append(toptext)
            toptext = toptext

        bottomtext = meme["bottomText"]
        if(bottomtext):
            bottomtext_texts.append(bottomtext)
            bottomtext = bottomtext

        data = {"Toptext": toptext,"Bottomtext": bottomtext, "FileName":visual_filename, "Topics":meme["topics"], "CreatedAt":meme["createdAt"]}
        
        response = session.post(meme_url,files = {"VisualFile": visual_file.content}, data = data)
    if(not response.ok):
        print(visual_file, response)
    else:
        print("okay", meme["id"])  

print(len(visuals_filenames))
print(len(meme_ids))
print(len(toptext_texts))
print(len(bottomtext_texts))
# Closing file
f.close()