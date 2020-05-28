const fse = require('fs-extra');
const glob = require('glob');

const DIST_DIR = 'MorePlanning';

module.exports = (grunt) => {
    "use strict";

    grunt.loadNpmTasks('grunt-replace');

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),
        replace: {
            about: {
                options: {
                    patterns: [
                        {
                            match: 'version',
                            replacement: '<%= pkg.version %>'
                        }
                    ]
                },
                files: [
                    {src: 'About/About.xml', dest: '../' + DIST_DIR + '/About/About.xml'}
                ]
            },
            assembly: {
                options: {
                    patterns: [
                        {
                            match: /[0-9]+\.[0-9]+\.[0-9]+/g,
                            replacement: '<%= pkg.version %>'
                        }
                    ]
                },
                files: [
                    {src: 'Source/MorePlanning/Properties/AssemblyInfo.cs', dest: 'Source/MorePlanning/Properties/AssemblyInfo.cs'}
                ]
            }
        }
    });

    // Setup project
    grunt.registerTask('setup', () => {
        try {
            // Copy default dlls for modding from game folder
            fse.copySync('../../RimWorldWin64_Data/Managed/Assembly-CSharp.dll', 'Source/MorePlanning/Library/Assembly-CSharp.dll');
            let unityEngineDll = glob.sync('../../RimWorldWin64_Data/Managed/UnityEngine*.dll');
            for (let i = 0; i < unityEngineDll.length; i++) {
                let splited = unityEngineDll[i].split('/');
                let file = splited[splited.length - 1];
                fse.copySync(unityEngineDll[i], 'Source/MorePlanning/Library/' + file);
            }

            // Copy HugsLib.dll from workshop folder
            let filesFound = glob.sync('../../../../workshop/content/**/HugsLib.dll');
            if (filesFound.length > 0) {
                fse.copySync(filesFound[0], 'Source/MorePlanning/Library/HugsLib.dll');
            }
			
			// Copy 0Harmony.dll from workshop folder
            filesFound = glob.sync('../../../../workshop/content/**/0Harmony.dll');
            if (filesFound.length > 0) {
                fse.copySync(filesFound[0], 'Source/MorePlanning/Library/0Harmony.dll');
            }
        } catch (err) {
            console.error(err);
        }
    });

    // Copy files used in distribution
    grunt.registerTask('copy-dist-files', () => {
        fse.ensureDirSync('../' + DIST_DIR);
        
        let folders = [
            'About', 
            'Assemblies',
            'Defs',
            'Languages',
            'Textures',
        ];
        
        folders.forEach((folder) => {
            fse.copySync(folder, '../' + DIST_DIR + '/' + folder);
        });
    });
    
    // Compile project
    grunt.registerTask('compile', () => {
        const execSync = require('child_process').execSync;
        execSync('VsDevCmd && msbuild Source/MorePlanning');
        
        console.log('Versao ' + grunt.config.get('pkg').version + ' compilada');
    });
    
    grunt.registerTask('build-dist', [
        'replace:assembly',
        'compile',
        'copy-dist-files',
        'replace:about'
    ]);
    
    grunt.registerTask('build', [
        'replace:assembly',
        'compile'
    ]);

    grunt.registerTask('default', ['setup']);

};
