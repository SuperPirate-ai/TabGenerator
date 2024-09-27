import pygame
from fastapi import FastAPI
from time import perf_counter_ns as time_ns
from traceback import print_exc
import numbers

colors = [
    (255, 0  , 0  ), # red
    (0  , 255, 0  ), # green
    (0  , 0  , 255), # blue
    (255, 255, 0  ), # yellow
    (255  , 0  , 255), # pink
    (0  , 255, 255), # cyan

]
WIDTH = 1500
X_BORDER = 10
HEIGHT = 800
Y_BORDER = 10

class DataHolder:
    data = []
def start_pg():
    t = time_ns() ## start time for fps calculation
    pygame.init()
    canvas = pygame.display.set_mode((WIDTH + X_BORDER * 2, HEIGHT + Y_BORDER * 2))
    error_messages = pygame.Surface((HEIGHT + Y_BORDER * 2, WIDTH + X_BORDER * 2), pygame.SRCALPHA) ## canvas for error messages
    font = pygame.font.Font(None, 36)
    while True:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                return
        if DataHolder.data is None:
            continue
        canvas.fill((0, 0, 0)) ## clear the canvas
        error_messages.fill((0, 0, 0, 0)) ## clear the error messages
        err_or_warning_msg_idx = 1 ## index for error messages
        x_group_max_deviation = {} ## max deviation for each x scaling group
        y_group_max_deviation = {} ## max deviation for each y scaling group
        values = [] ## graphs, lines and points to be drawn
        for x_scale_group, y_scale_group, v in DataHolder.data: ## iterate over the data
            if v is None: ## Ignore None values
                continue
            if not isinstance(v, numbers.Number) and len(v) == 0: ## Ignore empty lists
                continue
            if y_scale_group == 0 and not isinstance(v, numbers.Number): ## Ignore non-number values for y_scale_group == 0 (supposed to be vertical line)
                err_or_warning_msg_idx += 1
                error_messages.blit(font.render(f"An Entry with YSG 0 (reserved for vertical lines) is not a number but instance of {type(v)}", 1, (255, 0, 0, 255)), (10, 30*err_or_warning_msg_idx))
                continue
            if x_scale_group not in x_group_max_deviation: ## Initialize max deviation for x_scale_group if not already initialized
                x_group_max_deviation[x_scale_group] = 0
            if y_scale_group not in y_group_max_deviation and y_scale_group != 0: ## Initialize max deviation for y_scale_group except when having vertical lines
                y_group_max_deviation[y_scale_group] = 0
            if y_scale_group == 0: 
                xmaxdev = abs(v) ## For vertical lines, the horizontal deviation is the value itself
            else:
                if not isinstance(v, list):
                    xmaxdev = 1 ## For horizontal lines, the horizontal deviation is 1
                else:
                    xmaxdev = len(v)-1 ## For multiple lines, the horizontal deviation is the count of the lines minus 1
            x_group_max_deviation[x_scale_group] = max(x_group_max_deviation[x_scale_group], abs(xmaxdev)) ## Update the max deviation for x_scale_group
            if y_scale_group != 0:
                if not isinstance(v, list):
                    ymaxdev = abs(v) ## For horizontal lines, the vertical deviation is the value itself
                else:
                    ymaxdev = max(abs(max(v)), abs(min(v))) ## For multiple lines, the vertical deviation is the maximum value
                y_group_max_deviation[y_scale_group] = max(y_group_max_deviation[y_scale_group], abs(ymaxdev)) ## Update the max deviation for y_scale_group
            values.append((x_scale_group, y_scale_group, v)) ## Append the graph, line or point to the list of values to be drawn

        for idx, (x_scale_group, y_scale_group, arr_or_x_val) in enumerate(values):
            if x_group_max_deviation[x_scale_group] == 0:
                err_or_warning_msg_idx += 1
                error_messages.blit(font.render(f"The point at y={arr_or_x_val} couldn't be shown (horizontal lines are represented using only integers, not iterables) as it was not in a x scaling group to be compared with on the x axis!", 1, (255, 0, 0, 255)), (10, 30*err_or_warning_msg_idx))
                continue
            x_scale = WIDTH / x_group_max_deviation[x_scale_group] ## calculate the x_scaling factor for the current values in arr_or_x_val
            if y_scale_group == 0: ## vertical line
                pygame.draw.line(canvas, colors[idx % len(colors)], (int(arr_or_x_val * x_scale + X_BORDER), Y_BORDER,),
                                (int(arr_or_x_val * x_scale + X_BORDER), HEIGHT + Y_BORDER), 2)              
                continue
            elif isinstance(arr_or_x_val, numbers.Number): ## single y values become a line parallel to the x-axis
                arr_or_x_val = [arr_or_x_val, arr_or_x_val]
                x_scale = WIDTH
            if y_group_max_deviation[y_scale_group] == 0: 
                y_scale = 0 ## if there is no deviation, the point or line(s) are on the x-axis
            else:
                y_scale = HEIGHT / y_group_max_deviation[y_scale_group] / 2 ## otherwise scale the y values to the height of the canvas
            if len(arr_or_x_val) > 1:
                pygame.draw.lines(canvas, colors[idx % len(colors)], False, [(int(x * x_scale + X_BORDER), int(-y * y_scale + HEIGHT/2 + Y_BORDER)) for x, y in enumerate(arr_or_x_val)], 2) ## draw lines between the points
            for x, y in enumerate(arr_or_x_val): ## iterate over the x and y values
                pygame.draw.circle(canvas, colors[idx % len(colors)], (int(x * x_scale + X_BORDER), int(-y * y_scale + HEIGHT/2 + Y_BORDER)), 5)
        canvas.blit(font.render(f"fps: {1e9/(time_ns()-t):.2f} with {sum([len(x) if isinstance(x, list) else 1 for _,_,x in values])} verticies", 1, (255, 255, 255)), (10, 30)) ## draw the fps on the canvas
        t = time_ns() ## start time for fps calculation
        ## put the error message canvas onto the normal canvas
        canvas.blit(error_messages, (0, 0))
        pygame.display.flip()        


if __name__ == "__main__":
    app = FastAPI()

    @app.post("/plot_data")
    async def plot_data(data: dict):
        t = time_ns()
        DataHolder.data = data.get("plotting_data")
        return "ok", 200
    import uvicorn
    import threading
    threading.Thread(target=start_pg).start()
    print("starting")
    uvicorn.run(app, host="0.0.0.0", port=5001)
 