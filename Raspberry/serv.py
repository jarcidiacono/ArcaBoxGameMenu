## File     : serv.py
 # Autor    : Jérémie Arcidiacono
 # Date     : October 2021
 # Description: Raspberry that controls a 7-segment LED display. It waits for HTTP requests to know when to start and stop the timer that appears on the display
##

from flask import Flask, Response, request
import board
from adafruit_ht16k33.segments import BigSeg7x4
from time import sleep
from multiprocessing import Process, Value, Array

# Init display
i2c = board.I2C()
display = BigSeg7x4(i2c, address=0x70) # DON'T FORGET to change the address, it can change with each installation

# Init Flask web server
app = Flask(__name__)

DEFAULT_TIME = [0,2,0,0] # The time allowed to each player (formats : mm:ss = [m, m, s, s]

time = Array('i', DEFAULT_TIME)
timerStatus = Value('i', 0)

# Set display settings
display.brightness = 0.5
display.blink_rate = 0
display.fill(0)

# Return a Flask object to submit a response to an HTTP request
# string status : response message
# int    code   : HTTP response code
def setResponse(status, code):
    return Response('{"info":"'+status+'"}', status=code, mimetype='application/json')

# Recursiv func to set the time one second lower
# int arrIndex : Index of the array that contain the time
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


# Flask listener (use the param GET 'a' to determine the action
@app.route('/timerStatus', methods=['GET'])
def index():
    global timerStatus
    global time
    action = request.args.get("a").lower()
    if action == 'start':
        time = Array('i', DEFAULT_TIME) # reset the time
        timerStatus.value = 1 # launch the timer
        display.fill(0)
        return setResponse('ok', 200)
    elif action == 'stop':
        time = Array('i', DEFAULT_TIME)
        timerStatus.value = 0
        display.fill(0)
        return setResponse('ok', 200)
    else:
        # error : unknow action
        return setResponse('Argument invalid', 400)

# Parallel process of the app
# It runs every second. It update the display if the timer is started
def timerLoop(timerStatus, time):
    while True:
        if timerStatus.value == 1:
            display.print(''.join(map(str, time))) # Display the time
            display.colon = True
            oneSecondLower(len(time)-1)
            sleep(1)
            if sum(time) < 1:
                # End-of-time animation
                for i in range(5):
                    display.fill(1)
                    sleep(0.5)
                    display.fill(0)
                    sleep(0.5)
                timerStatus.value = 0
                

# Start the two process (Flask listener + display updater)
if __name__ == '__main__':
    p = Process(target=timerLoop, args=(timerStatus,time))
    p.start() # Start the parallel process
    app.run(host='0.0.0.0', port=80, debug=False) # Start the Flask server
    p.join()

