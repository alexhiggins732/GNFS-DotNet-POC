
import os
import factmsieve
# Get the list of directories in the current directory
directory = '../RsaChallengePolynomials/bin/Debug/net6.0/RsaChallengeInputFiles'

# Get the list of entries (files and directories) in the specified directory
entries = os.listdir(directory)

# Filter entries based on names that start with 'RSA-' and are files
work_files = [name for name in entries if name.startswith('RSA-') and os.path.isfile(os.path.join(directory, name))]

# Print the resulting list of directories
for work_file in work_files:
    print(work_file)
