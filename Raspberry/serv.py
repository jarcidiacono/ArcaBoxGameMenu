from flask import Flask, Response, request
import board
from adafruit_ht16k33.segments import BigSeg7x4
from time import sleep
from multiprocessing import Process, Value, Array

i2c = board.I2C()
display = BigSeg7x4(i2c, address=0x70)
app = Flask(__name__)

DEFAULT_TIME = [0,0,1,0] # The time allowed to each player

time = Array('i', DEFAULT_TIME) # first initialize
timerStatus = Value('i', 0)

display.brightness = 1
display.blink_rate = 0
display.fill(0)


def setResponse(status, code):
    return Response('{"info":"'+status+'"}', status=code, mimetype='application/json')

def oneSecondLower(arrIndex):
    global time
    if arrIndex < 0:
        return 0
    else:
        time[arrIndex] -= 1
        if time[arrIndex] < 0:
            if arrIndex == 2:
                time[arrIndex] = 5
            else:
                time[arrIndex] = 9
            oneSecondLower(arrIndex-1)


@app.route('/timerStatus', methods=['GET'])
def index():
    global timerStatus
    global time
    action = request.args.get("a").lower()
    if action == 'start':
        time = Array('i', DEFAULT_TIME)
        timerStatus.value = 1
        print('new : ' + str(timerStatus.value))
        return setResponse('ok', 200)
    elif action == 'stop':
        time = Array('i', DEFAULT_TIME)
        timerStatus.value = 0
        print('new : ' + str(timerStatus.value))
        return setResponse('ok', 200)
    else:
        # error : unknow action
        return setResponse('Argument invalid', 400)


def timerLoop(timerStatus, time):
    while True:
        if timerStatus.value == 1:
            display.print(''.join(map(str, time)))
            oneSecondLower(len(time)-1)
            sleep(1)
            if sum(time) < 1:
                display.print(0000)
                sleep(3)
                display.fill(0)
                timerStatus.value = 0
                

if __name__ == '__main__':
    p = Process(target=timerLoop, args=(timerStatus,time))
    p.start()
    app.run(host='0.0.0.0', port=80, debug=False)
    p.join()

