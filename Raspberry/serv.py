from flask import Flask, Response, request

# App func
def getResponse(status, code):
    return Response('{"info":"'+status+'"}', status=code, mimetype='application/json')

app = Flask(__name__)



@app.route('/timerStatus', methods=['GET'])
def index():
    action = request.args.get("a").lower()
    if action == 'start':
        # ...
        return getResponse('ok', 200)
    elif action == 'stop':
        # ...
        return getResponse('ok', 200)
    else:
        # error : unknow action
        return getResponse('Argument invalid', 400)

@app.route('/aaa')
def page1():
    return Response('', status=402, mimetype='application/json')


if __name__ == '__main__':
    #app.run(debug=True, host='0.0.0.0')
    app.run(host='0.0.0.0', port=80, debug=False)