#include "Core/Core.h"
#include <string>
#include <fstream>
#include <vector>

std::vector<char> Core::readFileBinary(const std::string& filename) {
    CoreLogger::log("Trying to open file on path ", false);
    CoreLogger::log(filename);

    std::ifstream file(filename, std::ios::ate | std::ios::binary);

    if (!file.is_open()) {
        throw std::runtime_error("Failed to open file");
    }

    size_t fileSize = static_cast<size_t>(file.tellg());
    std::vector<char> buffer(fileSize);

    file.seekg(0);
    file.read(buffer.data(), fileSize);

    file.close();

    return buffer;
}
