import re
import sys
import base64
import democrite
import lxml.html
import urllib.request

def extract_article_content(command: democrite.vgrainCommand, tools: democrite.vgrainTools):
    """ Eval and calculate the mathematical result """
    html_page_url = command.get_content()

    if html_page_url is None:
        tools.get_logger.logWarning("null request");
        return {
            "Content" : "",
            "Source" : ""
        }
    
    try:    
        with urllib.request.urlopen(html_page_url) as f:
            html = f.read().decode('utf-8')

        t = lxml.html.fromstring(html)
        content = t.text_content()

        # Clean
        content = re.sub(r'\n[\n\s]+', '##SEPARATOR_PART##', content)
        parts = content.split('##SEPARATOR_PART##')

        cleanContent = []
    
        for msg in parts:

            msg = msg.strip()
            size = len(msg)
            if "function(" in msg or size < 50 or msg.count(' ') < 15:
                continue

            cleanContent.append(msg)

        content = "\n".join(cleanContent)

        tools.get_logger.logInformation("Source = '" + html_page_url + "' Result: " + content)

        return {
            "Content" : content,
            "Source" : html_page_url
        }

    except Exception as ex:
        tools.get_logger.logError("Source = '" + html_page_url + "' Error : " + str(ex))
        return {
            "Content" : base64.b64encode(str(ex).encode('utf8')).decode('utf8'),
            "Source" : html_page_url
        }
        

app = democrite.vgrain(sys.argv)
app.run(extract_article_content)

# app.test("https://news.mit.edu/2024/mit-engineers-grow-high-rise-3d-chips-1218", extract_article_content);