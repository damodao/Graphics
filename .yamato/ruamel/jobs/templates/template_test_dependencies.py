from ruamel.yaml.scalarstring import DoubleQuotedScalarString as dss
from ..shared.namer import *
from ..shared.constants import PATH_UNITY_REVISION, NPM_UPMCI_INSTALL_URL, UNITY_DOWNLOADER_CLI_URL, get_editor_revision
from ..shared.yml_job import YMLJob

class Template_TestDependenciesJob():
    
    def __init__(self, template, platform, editor):
        self.job_id = template_job_id_test_dependencies(template["id"],platform["os"],editor["track"])
        self.yml = self.get_job_definition(template,platform, editor).get_yml()


    def get_job_definition(yml, template, platform, editor):
    
        # define dependencies
        dependencies = [f'{templates_filepath()}#{template_job_id_test(template["id"],platform["os"],editor["track"])}']
        dependencies.extend([f'{packages_filepath()}#{package_job_id_pack(dep)}' for dep in template["dependencies"]])
        if str(editor['track']).lower() == 'custom-revision':
            dependencies.extend([f'{editor_priming_filepath()}#{editor_job_id(editor["track"], platform["os"]) }'])
        
        # define commands
        commands =  [
                f'npm install upm-ci-utils@stable -g --registry {NPM_UPMCI_INSTALL_URL}',
                f'pip install unity-downloader-cli --index-url {UNITY_DOWNLOADER_CLI_URL} --upgrade',
                f'unity-downloader-cli -u {get_editor_revision(editor, platform["os"])} -c editor --wait --published-only']
        if template.get('hascodependencies', None) is not None:
            commands.append(platform["copycmd"])
        commands.append(f'upm-ci template test -u {platform["editorpath"]} --type updated-dependencies-tests --project-path {template["packagename"]}')


        # construct job
        job = YMLJob()
        job.set_name(f'Test { template["name"] } {platform["name"]} {editor["track"]} - dependencies')
        job.set_agent(platform['agent_package'])
        job.add_dependencies(dependencies)
        job.add_commands(commands)
        job.add_artifacts_test_results()
        return job
    
    
    