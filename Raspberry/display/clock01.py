import board
from adafruit_ht16k33.segments import BigSeg7x4
from time import sleep

i2c = board.I2C()
display = BigSeg7x4(i2c, address=0x70)

print('start...')

display.brightness = 0.5
display.blink_rate = 0

time = [1, 0, 1, 0] # enter a number of minutes and secondes (10:21 = [1, 0, 2, 1])

def oneSecondLower(arrIndex):
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

while(sum(time)>0):
    sleep(1)
    display.print(''.join(map(str, time)))
    oneSecondLower(3)