#pragma once
#include "Logger/Logger.h"
#include <vector>

namespace Core {
	std::vector<char> readFileBinary(const std::string& filename);
};