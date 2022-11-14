## File     : serv.py
 # V1 :
 #      Author : Jérémie Arcidiacono
 #      Date   : October 2021
 # V2 :
 #      Author : CFPTI EE 2022-2023
 #      Date   : October-November 2022
 # Description: Raspberry that controls a 7-segment LED display. It waits for HTTP requests to know when to start and stop the timer that appears on the display
##

from cmath import isnan
from posixpath import split
from flask import Flask, Response, request
import board
from adafruit_ht16k33.segments import BigSeg7x4
from time import sleep
import multiprocessing
from multiprocessing import Process, Value, Array, active_children

# Init display
i2c = board.I2C()
display = BigSeg7x4(i2c, address=0x70) # DON'T FORGET to change the address, it can change with each installation

# Init Flask web server
app = Flask(__name__)

defaultTime = [0,0,0,5] # The time allowed to each player (formats : mm:ss = [m, m, s, s])     /!\ This is a default value! The raspberry will override the default value if another time value is received via HTTP

time = Array('i', defaultTime)
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
    global defaultTime
    if arrIndex < 0:
        return 0
    else:
        defaultTime[arrIndex] -= 1
        if defaultTime[arrIndex] < 0:
            if arrIndex == 2:
                defaultTime[arrIndex] = 5
            else:
                defaultTime[arrIndex] = 9
            oneSecondLower(arrIndex-1)


# Flask listener to start and stop the timer(use the param GET 'a' to determine the action)
@app.route('/timerStatus', methods=['GET'])
def index():
    global defaultTime
    global timerStatus
    global time
    action = request.args.get("a").lower()
    seconds = int(request.args.get("seconds"))
    if isnan(seconds):
        pass
    else:
        mm, ss = divmod(seconds, 60) # Transform seconds into minutes and seconds ( Example: 90 --> mm = 1 and ss = 30)
        if mm < 99:
            if(len(str(mm)) > 1 ) :
                defaultTime[0] = int([*str(mm)][0])
            else :
                defaultTime[0] = 0

            if(len(str(mm)) > 1 ) :
                defaultTime[1] = int([*str(mm)][1])
            else :
                defaultTime[1] = mm
                
            if(len(str(ss)) > 1 ) :
                defaultTime[2] = int([*str(ss)][0])
            else :
                defaultTime[2] = 0

            if(len(str(ss)) > 1 ) :
                defaultTime[3] = int([*str(ss)][1])
            else :
                defaultTime[3] = ss
    if action == 'start':
        time = Array('i', defaultTime) # reset the time
        timerStatus.value = 1 # launch the timer
        display.fill(0)
        active = active_children()
        for child in active :
            child.terminate()
        p = Process(target=timerLoop, args=(timerStatus,defaultTime))
        p.start() # Start the parallel process
        return setResponse('ok', 200)
    elif action == 'stop':
        active = active_children()
        for child in active :
            child.terminate()

        time = Array('i', defaultTime)
        timerStatus.value = 0
        display.fill(0)
        return setResponse('ok', 200)
    else:
        # error : unknow action
        return setResponse('Argument invalid', 400)

# Flask listener to define how long the timer will run (use the param GET 'second')
#@app.route('/timerInterval', methods=['GET'])
#def index():
#    global defaultTime
#    seconds = request.args.get("seconds")
#    if isnan(seconds):
#        # error : number of seconds invalid
#        return setResponse('Argument invalid', 400)
#    else:
#        mm, ss = divmod(seconds, 60) # Transform seconds into minutes and seconds ( Example: 90 --> mm = 1 and ss = 30)
#        if mm > 99:
#            # Time too big
#            return setResponse('Argument invalid: number of seconds is too high', 400)
#        defaultTime = (str(mm) + str(ss)).split('') # convert mm and ss into array
#        return setResponse('ok', 200)

# Parallel process of the app
# It runs every second. It update the display if the timer is started
def timerLoop(timerStatus, time):
    while True:
        if timerStatus.value == 1:
            global defaultTime
            time = defaultTime
            print(time)
            display.print(''.join(map(str, time))) # Display the time
            display.colon = True
            oneSecondLower(3)
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
    
    app.run(host='0.0.0.0', port=80, debug=True) # Start the Flask server
    #p.join()
