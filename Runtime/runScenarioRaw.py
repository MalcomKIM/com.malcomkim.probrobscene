import sys,os
import platform

if platform.system() == 'Windows':
    sys.path.append(os.path.dirname(__file__)+'/../ProbRobSceneEnvironment~')
else:
    sys.path.append(os.path.dirname(__file__)+'/../ProbRobSceneEnvironment-Linux~')


import probRobScene


if __name__ == "__main__":

    if len(sys.argv) != 3:
        print("Usage: python runScenarioRaw.py <scenario-file> <max-generations>")
        sys.exit(0)

    scenario_file = sys.argv[1]
    max_generations = int(sys.argv[2])

    scenario = probRobScene.scenario_from_file(scenario_file)

    for i in range(max_generations):
        print(f"Generation {i}")
        ex_world, used_its = scenario.generate(verbosity=2)
        ex_world.show_3d()
